# Criminal AI Project

Professional multi-repository workspace combining an Angular frontend, an ASP.NET Core Web API backend, and a Python-based AI training engine for individual face recognition.

This repository contains three complementary components:

- `Criminal_AI_Project_Angular` — Angular 18 frontend UI (NobleUI theme). Serves the administration/dashboard and user interface.
- `Criminal_AI_Project_API` — ASP.NET Core Web API providing endpoints to manage criminals metadata, images, and receive training notifications. Includes JWT-auth scaffolding, CORS policy and Swagger/OpenAPI setup.
- `Criminal_AI_Project_Python` — Lightweight Flask service that trains an LBPH face recognizer from images stored in the API project's `wwwroot/images/criminals` folder. Notifies the API when training completes.

---

## Quick overview

This combined README documents how to get the whole stack running locally on a Windows machine (PowerShell). The intended dev workflow is:

1. Run the ASP.NET Core API.
2. Run the Python trainer service (it reads images from the API `wwwroot` folder).
3. Serve the Angular frontend for the administrative UI.

The Angular app talks to the API for data and authentication. The Python trainer notifies the API when training finishes.

---

## Requirements

- Windows 10/11 (or WSL if preferred)
- .NET 8 SDK (recommended; check the `Criminal_AI_Project_API` project target)
- Node.js 18+ and npm (for Angular)
- Python 3.10+ and pip
- OpenCV for Python (cv2) and supporting packages
- Optional: Visual Studio / VS Code for IDE experience

---

## Setup & run (Windows PowerShell)

NOTE: adjust paths and ports if your environment differs.

### 1) API (ASP.NET Core)

1. Open PowerShell and navigate to the API folder:

```powershell
cd "E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_API\Criminal_AI_Project_API"
```

2. Restore and run with dotnet:

```powershell
dotnet restore
dotnet run
```

The API will start and, in development, expose Swagger UI (by default when `ASPNETCORE_ENVIRONMENT` is `Development`). The `Program.cs` shows it uses JWT authentication and a permissive CORS policy named `AllowAll`.

Default JWT config keys are read from configuration but fallback to reasonable placeholders (see `Program.cs`). Ensure to set `appsettings.json` or environment variables for production use.

### 2) Python trainer service

1. Create and activate a virtual environment (recommended):

```powershell
cd "E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_Python\Python_Engine"
python -m venv .venv; .\.venv\Scripts\Activate.ps1
```

2. Install dependencies (create a `requirements.txt` if you prefer):

```powershell
pip install flask flask-cors opencv-python-headless numpy requests
```

3. Verify the `IMAGES_DIR` constant at the top of `training_api.py` points to the API project's `wwwroot/images/criminals` folder. By default it is set to:

```
E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_API\Criminal_AI_Project_API\wwwroot\images\criminals
```

4. Run the trainer:

```powershell
python training_api.py
```

The trainer listens on port 5001 and exposes a POST `/train` endpoint. When training completes it POSTs a notification to the API at `http://localhost:5263/api/Criminals/training` (configurable via `NOTIFICATION_API_BASE_URL`).

Notes:
- The trainer uses OpenCV LBPH recognizer and expects faces in images. Filenames (without extension) are used as GUIDs for each individual.
- Ensure `haarcascade_frontalface_default.xml` is present in the Python Engine folder. The trained model files are written to `model_files/`.

### 3) Angular frontend

1. Install dependencies and run the frontend:

```powershell
cd "E:\HackAura\Criminal_AI_Project\Criminal_AI_Project_Angular"
npm install
npm start
```

2. The Angular app runs with the Angular CLI (default `ng serve`) and will be available on `http://localhost:4200` unless configured otherwise.

The app's `package.json` shows Angular 18 and a set of UI dependencies used by the NobleUI theme. Build the app for production with `npm run build`.

---

## Project structure (high-level)

- `Criminal_AI_Project_Angular/` — front-end source, `package.json` lists Angular dependencies and scripts.
- `Criminal_AI_Project_API/` — ASP.NET Core Web API, Serilog logging, JWT setup, Swagger, CORS. Static images are served from `wwwroot`.
- `Criminal_AI_Project_Python/` — `Python_Engine/training_api.py`, OpenCV cascade and model files in `model_files/`.

---

## Configuration & environment

- API settings: use `appsettings.json` and `appsettings.Development.json` for connection strings, JWT keys, and ports.
- Python trainer: edit the constants at the top of `training_api.py` for host/port, image directory path, and notification API URL.
- Angular: adjust environment files in `src/environments/` to point to the API base URL and toggle production flags.

---

## Security notes

- Replace any placeholder JWT secrets with secure values in production and keep them out of source control (use environment variables or secret stores).
- The API currently enables a permissive CORS policy `AllowAll` for development; restrict this in production.
- Validate and sanitize uploaded images, and limit file sizes where applicable.

---

## Testing and development tips

- Use the API's Swagger UI to explore endpoints and test the notification endpoint from the Python trainer.
- For the Python trainer, add a few known face images named with unique GUIDs to the `wwwroot/images/criminals` folder and call `POST http://localhost:5001/train` to generate `model_files/classifier.xml` and metadata.

---

## Contribution

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature`.
3. Make changes and add tests where appropriate.
4. Open a pull request describing the change.

---

## License

Include your preferred license file in the root (e.g., `LICENSE`). This README assumes a permissive open-source workflow; adjust per your legal needs.

---

## Contact

For questions or issues, open an issue on the repository or contact the maintainer(s) listed in project metadata.

---

Last updated: 2025-10-05
