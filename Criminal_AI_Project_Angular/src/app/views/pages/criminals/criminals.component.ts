import { Component, OnInit } from '@angular/core';
import { CriminalsService, Criminal, DetectedCriminal } from '../../../core/services/criminals.service';
import { NgxDatatableModule } from '@siemens/ngx-datatable';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CriminalFormComponent } from './modals/criminal-form/criminal-form.component';
import { SieveModel } from '../../../core/models/sieve.model';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-criminals',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NgxDatatableModule,
    FormsModule
  ],
  templateUrl: './criminals.component.html',
  styleUrl: './criminals.component.scss'
})
export class CriminalsComponent implements OnInit {

  public rows: Criminal[] = [];
  public selectedDetectedCriminal: DetectedCriminal | null = null;
  public detectionDataState: 'initial' | 'loading' | 'found' | 'notFound' | 'error' = 'initial';
  public totalRecords: number = 0;
  public sieveModel: SieveModel = {
    page: 1,
    pageSize: 10,
    sorts: '',
    filters: ''
  };
  public searchValue: string = '';
  public baseUrl = environment.baseUrl;

  constructor(
    private criminalsService: CriminalsService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.getCriminals();
  }

  viewDetections(guid: string) {
    this.detectionDataState = 'loading';
    this.selectedDetectedCriminal = null;
    this.criminalsService.getDetectedCriminalByGuid(guid).subscribe({
      next: (data: DetectedCriminal) => {
        if (data && data.sessions && data.sessions.length > 0) {
          this.selectedDetectedCriminal = data;
          this.detectionDataState = 'found';
        } else {
          this.detectionDataState = 'notFound';
        }
      },
      error: () => {
        this.detectionDataState = 'error';
        Swal.fire(
          'Error!',
          'Failed to fetch detection data for this criminal.',
          'error'
        );
      }
    });
  }

  getCriminals() {
    this.criminalsService.getCriminals(this.sieveModel).subscribe((data: any) => {
      this.rows = data.items;
      this.totalRecords = data.totalCount;
    });
  }

  addCriminal() {
    const modalRef = this.modalService.open(CriminalFormComponent, { size: 'lg' });
    modalRef.result.then((result) => {
      if (result) {
        this.getCriminals(); // Refresh the list
      }
    }).catch((error) => {
      console.log('Modal dismissed with error: ', error);
    });
  }

  setPage(pageInfo: any) {
    this.sieveModel.page = pageInfo.offset + 1;
    this.getCriminals();
  } 

  search() {
    this.sieveModel.filters = `criminalName@=*${this.searchValue}`;
    this.getCriminals();
  }

  editCriminal(criminal: Criminal) {
    const modalRef = this.modalService.open(CriminalFormComponent, { size: 'lg' });
    modalRef.componentInstance.criminal = criminal;
    modalRef.result.then((result) => {
      if (result) {
        this.getCriminals(); // Refresh the list
      }
    }).catch((error) => {
      console.log('Modal dismissed with error: ', error);
    });
  }

  deleteCriminal(criminal: Criminal) {
    Swal.fire({
      title: 'Are you sure?',
      text: `You are about to delete ${criminal.criminalName}. This action cannot be undone.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.criminalsService.deleteCriminal(criminal.guid).subscribe({
          next: () => {
            Swal.fire(
              'Deleted!',
              'The criminal record has been deleted.',
              'success'
            );
            this.getCriminals(); // Refresh the list
          },
          error: (error) => {
            Swal.fire(
              'Error!',
              'Failed to delete the criminal record.',
              'error'
            );
          }
        });
      }
    });
  }
}
