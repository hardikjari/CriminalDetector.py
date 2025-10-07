import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl; 

  constructor(private http: HttpClient) { }

  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Criminals/AdminLogin`, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem('authToken', response.token);
          this.storeAdminDetails(response.admin);
        }
      })
    );
  }

  storeAdminDetails(admin: any): void {
    localStorage.setItem('adminDetails', JSON.stringify(admin));
  }

  getAdminDetails(): any {
    const adminDetails = localStorage.getItem('adminDetails');
    return adminDetails ? JSON.parse(adminDetails) : null;
  }

  isLoggedIn(): boolean {
    // Check if the token exists and is not expired
    const token = localStorage.getItem('authToken');
    return !!token; // Simple check, you might want to add token expiration logic
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('adminDetails');
  }
}
