using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations
{
    public class CriminalsService : ICriminalsService
    {
    private readonly IGenericRepository<CriminalModel> _criminalRepo;
    private readonly IGenericRepository<CriminalCrimesModel> _criminalCrimesRepo;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;

        public CriminalsService(IGenericRepository<CriminalModel> criminalRepo,
            IGenericRepository<CriminalCrimesModel> criminalCrimesRepo,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory)
        {
            _criminalRepo = criminalRepo;
            _criminalCrimesRepo = criminalCrimesRepo;
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CriminalReadDTO> CreateAsync(CriminalCreateDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var model = new CriminalModel
            {
                Guid = Guid.NewGuid(),
                CriminalName = dto.CriminalName,
                Crime = dto.Crime,
                Location = dto.Location,
                DateOfCrime = dto.DateOfCrime,
                ImageUrl = null
            };

            // If an image base64 payload was provided, save it to wwwroot/images/criminals/{guid}.{ext}
            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                try
                {
                    string base64 = dto.ImageBase64.Trim();
                    string? ext = "jpg";
                    if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        // data:[<mediatype>][;base64],<data>
                        var comma = base64.IndexOf(',');
                        var meta = base64.Substring(5, comma - 5); // after 'data:'
                        var semi = meta.IndexOf(';');
                        var mime = semi > 0 ? meta.Substring(0, semi) : meta;
                        if (mime.Contains('/'))
                        {
                            var parts = mime.Split('/');
                            var possible = parts[1];
                            if (possible == "jpeg" || possible == "jpg") ext = "jpg";
                            else if (possible == "png") ext = "png";
                            else if (possible == "gif") ext = "gif";
                            else ext = possible;
                        }
                        base64 = base64.Substring(comma + 1);
                    }

                    var imagesRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath) ? _env.WebRootPath : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                    var imagesDir = System.IO.Path.Combine(imagesRoot, "images", "criminals");
                    System.IO.Directory.CreateDirectory(imagesDir);

                    var fileName = model.Guid.ToString() + "." + ext;
                    var filePath = System.IO.Path.Combine(imagesDir, fileName);
                    var bytes = Convert.FromBase64String(base64);
                    System.IO.File.WriteAllBytes(filePath, bytes);

                    model.ImageUrl = $"/images/criminals/{fileName}";
                }
                catch
                {
                    // If image processing fails, continue without image. Optionally log the error.
                    model.ImageUrl = null;
                }
            }

            await _criminalRepo.AddAsync(model);
            await _criminalRepo.SaveChangesAsync();

            // Persist crimes if provided
            if (dto.Crimes != null && dto.Crimes.Any())
            {
                foreach (var c in dto.Crimes)
                {
                    var cm = new CriminalCrimesModel
                    {
                        Guid = Guid.NewGuid(),
                        CriminalId = model.Id,
                        CrimeType = c.CrimeType,
                        CrimeDescription = c.CrimeDescription
                    };
                    await _criminalCrimesRepo.AddAsync(cm);
                }
                await _criminalCrimesRepo.SaveChangesAsync();
            }

            // After successful DB insert, call external train endpoint (best-effort)
            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = new
                {
                    guid = model.Guid,
                    id = model.Id,
                    name = model.CriminalName
                };
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                // fire-and-forget in a safe try/catch; await so we can observe failures if desired
                var resp = await client.PostAsync("http://127.0.0.1:5001/train", content).ConfigureAwait(false);
                try
                {
                    var respBody = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Log.Information("Train endpoint responded with {StatusCode}: {Body}", resp.StatusCode, respBody);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to read response body from train endpoint");
                }
            }
            catch
            {
                // swallow errors to avoid breaking main flow
            }

            return await GetByGuidAsync(model.Guid);
        }

        public async Task<IEnumerable<CriminalReadDTO>> GetAllAsync()
        {
            var all = await _criminalRepo.GetAllAsync();
            var ordered = all.OrderByDescending(c => c.DateOfCrime);

            var list = new List<CriminalReadDTO>();
            foreach (var c in ordered)
            {
                var crimes = await _criminalCrimesRepo.FindAsync(x => x.CriminalId == c.Id);
                list.Add(new CriminalReadDTO
                {
                    Guid = c.Guid,
                    CriminalName = c.CriminalName,
                    Crime = c.Crime,
                    Location = c.Location,
                    DateOfCrime = c.DateOfCrime,
                    ImageUrl = c.ImageUrl,
                    Crimes = crimes.Select(x => new CriminalCrimeReadDTO
                    {
                        Guid = x.Guid,
                        CriminalId = x.CriminalId,
                        CrimeType = x.CrimeType,
                        CrimeDescription = x.CrimeDescription
                    }).ToList()
                });
            }

            return list;
        }

        public async Task<Models.DTOs.PagedResult<CriminalReadDTO>> GetAllAsync(Models.DTOs.SieveModel sieve)
        {
            // Start with IQueryable for efficient queries
            var query = _criminalRepo.GetQueryable();

            // Apply simple search across common textual fields
            if (!string.IsNullOrWhiteSpace(sieve?.Search))
            {
                var s = sieve.Search.Trim();
                query = query.Where(c =>
                    (EF.Property<string>(c, "CriminalName") ?? string.Empty).Contains(s) ||
                    (EF.Property<string>(c, "Crime") ?? string.Empty).Contains(s) ||
                    (EF.Property<string>(c, "Location") ?? string.Empty).Contains(s)
                );
            }

            // Very small filter parser: supports semicolon-separated expressions like "Crime==Robbery;Location==NY"
            if (!string.IsNullOrWhiteSpace(sieve?.Filters))
            {
                var parts = sieve.Filters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    var expr = p.Trim();
                    if (expr.Contains("=="))
                    {
                        var idx = expr.IndexOf("==");
                        var field = expr.Substring(0, idx).Trim();
                        var value = expr.Substring(idx + 2).Trim();

                        var propertyNames = typeof(CriminalModel).GetProperties().Select(p => p.Name).ToList();
                        var match = propertyNames.FirstOrDefault(p => p.Equals(field, StringComparison.OrdinalIgnoreCase));
                        if (match == null) continue;

                        query = query.Where(c => EF.Property<string>(c, match) == value);
                    }
                    else if (expr.Contains("@=*"))
                    {
                        var idx = expr.IndexOf("@=*");
                        var field = expr.Substring(0, idx).Trim();
                        var value = expr.Substring(idx + 3).Trim();

                        var propertyNames = typeof(CriminalModel).GetProperties().Select(p => p.Name).ToList();
                        var match = propertyNames.FirstOrDefault(p => p.Equals(field, StringComparison.OrdinalIgnoreCase));
                        if (match == null) continue;

                        query = query.Where(c => (EF.Property<string>(c, match) ?? string.Empty).Contains(value));
                    }
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sieve?.Sorts))
            {
                var sorts = sieve.Sorts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                IOrderedQueryable<CriminalModel>? ordered = null;
                for (int i = 0; i < sorts.Length; i++)
                {
                    var s = sorts[i].Trim();
                    var desc = s.StartsWith("-");
                    var field = desc ? s.Substring(1) : s;
                    if (i == 0)
                    {
                        ordered = desc ? query.OrderByDescending(c => EF.Property<object>(c, field)) : query.OrderBy(c => EF.Property<object>(c, field));
                    }
                    else
                    {
                        ordered = desc ? ordered.ThenByDescending(c => EF.Property<object>(c, field)) : ordered.ThenBy(c => EF.Property<object>(c, field));
                    }
                }
                if (ordered != null) query = ordered;
            }
            else
            {
                // Default ordering
                query = query.OrderByDescending(c => EF.Property<DateTime>(c, "DateOfCrime"));
            }

            var total = await query.CountAsync();

            // Pagination
            var page = Math.Max(1, sieve?.Page ?? 1);
            var pageSize = Math.Max(1, Math.Min(100, sieve?.PageSize ?? 10));
            var skip = (page - 1) * pageSize;

            var pageItems = await query.Skip(skip).Take(pageSize).ToListAsync();

            // Project to DTOs and include crimes
            var list = new List<CriminalReadDTO>();
            foreach (var c in pageItems)
            {
                var crimes = await _criminalCrimesRepo.FindAsync(x => x.CriminalId == c.Id);
                list.Add(new CriminalReadDTO
                {
                    Guid = c.Guid,
                    CriminalName = c.CriminalName,
                    Crime = c.Crime,
                    Location = c.Location,
                    DateOfCrime = c.DateOfCrime,
                    ImageUrl = c.ImageUrl,
                    Crimes = crimes.Select(x => new CriminalCrimeReadDTO
                    {
                        Guid = x.Guid,
                        CriminalId = x.CriminalId,
                        CrimeType = x.CrimeType,
                        CrimeDescription = x.CrimeDescription
                    }).ToList()
                });
            }

            return new Models.DTOs.PagedResult<CriminalReadDTO>
            {
                Items = list,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CriminalReadDTO?> GetByGuidAsync(Guid guid)
        {
            var results = await _criminalRepo.FindAsync(c => c.Guid == guid);
            var c = results.FirstOrDefault();
            if (c == null) return null;

            var crimes = await _criminalCrimesRepo.FindAsync(x => x.CriminalId == c.Id);
            var dto = new CriminalReadDTO
            {
                Guid = c.Guid,
                CriminalName = c.CriminalName,
                Crime = c.Crime,
                Location = c.Location,
                DateOfCrime = c.DateOfCrime,
                ImageUrl = c.ImageUrl,
                Crimes = crimes.Select(x => new CriminalCrimeReadDTO
                {
                    Guid = x.Guid,
                    CriminalId = x.CriminalId,
                    CrimeType = x.CrimeType,
                    CrimeDescription = x.CrimeDescription
                }).ToList()
            };

            return dto;
        }

        public async Task<CriminalReadDTO?> UpdateAsync(Models.DTOs.CriminalUpdateDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Find the model by guid
            var results = await _criminalRepo.FindAsync(c => c.Guid == dto.Guid);
            var model = results.FirstOrDefault();
            if (model == null) return null;

            // Update simple fields if provided (null = no change)
            if (dto.CriminalName != null) model.CriminalName = dto.CriminalName;
            if (dto.Crime != null) model.Crime = dto.Crime;
            if (dto.Location != null) model.Location = dto.Location;
            if (dto.DateOfCrime.HasValue) model.DateOfCrime = dto.DateOfCrime.Value;

            // Handle image replacement if provided
            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                try
                {
                    // delete old image if exists
                    if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                    {
                        var rel = model.ImageUrl.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar);
                        var imagesRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath) ? _env.WebRootPath : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                        var physical = System.IO.Path.Combine(imagesRoot, rel);
                        if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
                    }

                    string base64 = dto.ImageBase64.Trim();
                    string? ext = "jpg";
                    if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = base64.IndexOf(',');
                        var meta = base64.Substring(5, comma - 5);
                        var semi = meta.IndexOf(';');
                        var mime = semi > 0 ? meta.Substring(0, semi) : meta;
                        if (mime.Contains('/'))
                        {
                            var parts = mime.Split('/');
                            var possible = parts[1];
                            if (possible == "jpeg" || possible == "jpg") ext = "jpg";
                            else if (possible == "png") ext = "png";
                            else if (possible == "gif") ext = "gif";
                            else ext = possible;
                        }
                        base64 = base64.Substring(comma + 1);
                    }

                    var imagesRoot2 = !string.IsNullOrWhiteSpace(_env.WebRootPath) ? _env.WebRootPath : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                    var imagesDir = System.IO.Path.Combine(imagesRoot2, "images", "criminals");
                    System.IO.Directory.CreateDirectory(imagesDir);

                    var fileName = model.Guid.ToString() + "." + ext;
                    var filePath = System.IO.Path.Combine(imagesDir, fileName);
                    var bytes = Convert.FromBase64String(base64);
                    System.IO.File.WriteAllBytes(filePath, bytes);

                    model.ImageUrl = $"/images/criminals/{fileName}";
                }
                catch
                {
                    // ignore image errors
                }
            }

            // Replace crimes if provided; else leave unchanged
            if (dto.Crimes != null)
            {
                // delete existing
                var existing = await _criminalCrimesRepo.FindAsync(x => x.CriminalId == model.Id);
                foreach (var ex in existing)
                {
                    _criminalCrimesRepo.Remove(ex);
                }
                await _criminalCrimesRepo.SaveChangesAsync();

                // add new
                foreach (var c in dto.Crimes)
                {
                    var cm = new CriminalCrimesModel
                    {
                        Guid = Guid.NewGuid(),
                        CriminalId = model.Id,
                        CrimeType = c.CrimeType,
                        CrimeDescription = c.CrimeDescription
                    };
                    await _criminalCrimesRepo.AddAsync(cm);
                }
                await _criminalCrimesRepo.SaveChangesAsync();
            }

            _criminalRepo.Update(model);
            await _criminalRepo.SaveChangesAsync();

            return await GetByGuidAsync(model.Guid);
        }

        public async Task<bool> DeleteByGuidAsync(Guid guid)
        {
            // Find the model by guid
            var results = await _criminalRepo.FindAsync(c => c.Guid == guid);
            var model = results.FirstOrDefault();
            if (model == null) return false;

            // Delete associated crime records
            var crimes = await _criminalCrimesRepo.FindAsync(x => x.CriminalId == model.Id);
            foreach (var cr in crimes)
            {
                _criminalCrimesRepo.Remove(cr);
            }
            await _criminalCrimesRepo.SaveChangesAsync();

            // Delete image file if exists
            if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                try
                {
                    var rel = model.ImageUrl.TrimStart('/').Replace('/', System.IO.Path.DirectorySeparatorChar);
                    var imagesRoot = !string.IsNullOrWhiteSpace(_env.WebRootPath) ? _env.WebRootPath : System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
                    var physical = System.IO.Path.Combine(imagesRoot, rel);
                    if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
                }
                catch
                {
                    // ignore file deletion errors
                }
            }

            _criminalRepo.Remove(model);
            await _criminalRepo.SaveChangesAsync();
            return true;
        }
    }
}
