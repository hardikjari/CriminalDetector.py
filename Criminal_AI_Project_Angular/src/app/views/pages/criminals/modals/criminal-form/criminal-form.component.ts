import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { FeatherIconDirective } from '../../../../../core/feather-icon/feather-icon.directive';
import { CriminalsService, Criminal } from '../../../../../core/services/criminals.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-criminal-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FeatherIconDirective],
  templateUrl: './criminal-form.component.html',
  styleUrls: ['./criminal-form.component.scss']
})
export class CriminalFormComponent implements OnInit {
  criminalForm: FormGroup;
  criminal: Criminal;
  maxDate: string;
  isSubmitting = false;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private criminalsService: CriminalsService
  ) {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const hh = String(today.getHours()).padStart(2, '0');
    const min = String(today.getMinutes()).padStart(2, '0');
    this.maxDate = `${yyyy}-${mm}-${dd}T${hh}:${min}`;
  }

  ngOnInit(): void {
    this.criminalForm = this.fb.group({
      guid: [''],
      criminalName: ['', Validators.required],
      crime: ['', Validators.required],
      location: ['', Validators.required],
      dateOfCrime: ['', Validators.required],
      imageBase64: [''],
      crimes: this.fb.array([])
    });

    if (this.criminal) {
      this.criminalForm.patchValue(this.criminal);
      this.crimes.clear();
      this.criminal.crimes.forEach(crime => {
        this.crimes.push(this.createCrime(crime.crimeType, crime.crimeDescription));
      });
    } else {
      this.addCrime();
    }
  }

  createCrime(crimeType: string = '', crimeDescription: string = ''): FormGroup {
    return this.fb.group({
      crimeType: [crimeType, Validators.required],
      crimeDescription: [crimeDescription, Validators.required]
    });
  }

  get crimes(): FormArray {
    return this.criminalForm.get('crimes') as FormArray;
  }

  addCrime(): void {
    this.crimes.push(this.createCrime());
  }

  removeCrime(index: number): void {
    this.crimes.removeAt(index);
  }

  getFileName(url: string): string {
    if (!url) {
      return '';
    }
    if (url.startsWith('data:')) {
      return 'image.png'; // Placeholder for data URLs
    }
    return url.substring(url.lastIndexOf('/') + 1);
  }

  onFileChange(event: any): void {
    const reader = new FileReader();
    if (event.target.files && event.target.files.length) {
      const [file] = event.target.files;
      reader.readAsDataURL(file);
      reader.onload = () => {
        const base64String = (reader.result as string).split(',')[1];
        this.criminalForm.patchValue({
          imageBase64: base64String
        });
      };
    }
  }

  onSubmit(): void {
    if (!this.criminalForm.valid || this.isSubmitting) {
      return;
    }
    this.isSubmitting = true;

    if (this.criminal) {
      const formValue = this.criminalForm.value;
      if (formValue.imageBase64) {
        this.updateCriminal(formValue);
      } else if (this.criminal.imageUrl) {
        this.urlToBase64(this.criminal.imageUrl).then(base64 => {
          formValue.imageBase64 = base64;
          this.updateCriminal(formValue);
        }).catch(error => {
           console.error('Error converting URL to base64:', error);
           Swal.fire({
            title: 'Error!',
            text: 'Failed to process existing image.',
            icon: 'error',
            confirmButtonText: 'OK'
          });
        });
      } else {
        this.updateCriminal(formValue);
      }
    } else {
      this.addCriminalRecord(this.criminalForm.value);
    }
  }

  private updateCriminal(criminalData: any): void {
    this.criminalsService.updateCriminal(this.criminal.guid, criminalData).subscribe({
      next: () => {
        this.activeModal.close(true);
        Swal.fire({
          title: 'Success!',
          text: 'Criminal updated successfully.',
          icon: 'success',
          confirmButtonText: 'OK'
        });
      },
      error: (error: any) => {
        this.isSubmitting = false;
        Swal.fire({
          title: 'Error!',
          text: 'Failed to update criminal.',
          icon: 'error',
          confirmButtonText: 'OK'
        });
      }
    });
  }

  private addCriminalRecord(criminalData: any): void {
    this.criminalsService.addCriminal(criminalData).subscribe({
      next: () => {
        this.activeModal.close(true);
        Swal.fire({
          title: 'Success!',
          text: 'Criminal added successfully.',
          icon: 'success',
          confirmButtonText: 'OK'
        });
      },
      error: (error: any) => {
        this.isSubmitting = false;
        Swal.fire({
          title: 'Error!',
          text: 'Failed to add criminal.',
          icon: 'error',
          confirmButtonText: 'OK'
        });
      }
    });
  }

  private async urlToBase64(url: string): Promise<string> {
    const response = await fetch(url);
    const blob = await response.blob();
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const result = reader.result as string;
        resolve(result.split(',')[1]);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }
}
