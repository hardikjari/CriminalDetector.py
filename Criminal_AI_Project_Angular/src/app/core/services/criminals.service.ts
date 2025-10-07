import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SieveModel } from '../models/sieve.model';

export interface Criminal {
  guid: string;
  criminalName: string;
  crime: string;
  location: string;
  dateOfCrime: string;
  imageUrl: string | null;
  crimes: Crime[];
}

export interface Crime {
  guid: string;
  criminalId: number;
  crimeType: string;
  crimeDescription: string;
}

export interface DetectedCriminalFile {
  fileName: string;
  url: string;
  size: number;
  lastModified: string;
}

export interface DetectedCriminalSession {
  session: string;
  files: DetectedCriminalFile[];
}

export interface DetectedCriminal {
  guid: string;
  sessions: DetectedCriminalSession[];
}

@Injectable({
  providedIn: 'root'
})
export class CriminalsService {
  private apiUrl = environment.apiUrl; // Replace with your actual API URL

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  getCriminals(sieveModel: SieveModel): Observable<any> {
    let params = new HttpParams();
    if (sieveModel.filters) {
      params = params.append('filters', sieveModel.filters);
    }
    if (sieveModel.sorts) {
      params = params.append('sorts', sieveModel.sorts);
    }
    if (sieveModel.page) {
      params = params.append('page', sieveModel.page.toString());
    }
    if (sieveModel.pageSize) {
      params = params.append('pageSize', sieveModel.pageSize.toString());
    }

    return this.http.get<any>(`${this.apiUrl}/Criminals/GetCriminals`, { headers: this.getHeaders(), params });
  }

  addCriminal(criminal: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Criminals`, criminal, { headers: this.getHeaders() });
  }

  updateCriminal(guid: string, criminal: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/Criminals/${guid}`, criminal, { headers: this.getHeaders() });
  }

  deleteCriminal(guid: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/Criminals/${guid}`, { headers: this.getHeaders() });
  }

  getCriminalDashboardData() : Observable<any> {
     return this.http.get<any>(`${this.apiUrl}/Criminals/dashboard`, { headers: this.getHeaders() });
  }

  getDetectedCriminalByGuid(guid: string): Observable<DetectedCriminal> {
    return this.http.get<DetectedCriminal>(`${this.apiUrl}/Criminals/detected/${guid}`, { headers: this.getHeaders() });
  }
}
