import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbCalendar, NgbDatepickerModule, NgbDateStruct, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ApexOptions, NgApexchartsModule } from "ng-apexcharts";
import { FeatherIconDirective } from '../../../core/feather-icon/feather-icon.directive';
import { ThemeCssVariableService, ThemeCssVariablesType } from '../../../core/services/theme-css-variable.service';
import { CriminalsService } from '../../../core/services/criminals.service';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../../layout/navbar/navbar.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    NgbDropdownModule,
    FormsModule,
    NgbDatepickerModule,
    NgApexchartsModule,
    FeatherIconDirective,
    CommonModule,
    NavbarComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {

  public dashboardData: any;
  public topCriminalsChartOptions: ApexOptions | any;

  themeCssVariables = inject(ThemeCssVariableService).getThemeCssVariables();

  constructor(private criminalsService: CriminalsService) {}

  ngOnInit(): void {
    this.criminalsService.getCriminalDashboardData().subscribe(data => {
      this.dashboardData = data;
      this.topCriminalsChartOptions = this.getTopCriminalsChartOptions(this.themeCssVariables, data.topCriminals);
    });
  }

  getTopCriminalsChartOptions(themeVariables: ThemeCssVariablesType, topCriminals: any[]) {
    const categories = topCriminals.map(c => c.criminalName);
    const seriesData = topCriminals.map(c => c.crimeCount);

    return {
      series: [{
        name: 'Crimes',
        data: seriesData
      }],
      chart: {
        type: 'bar',
        height: '330',
        parentHeightOffset: 0,
        foreColor: themeVariables.secondary,
        toolbar: {
          show: false
        },
        zoom: {
          enabled: false
        }
      },
      colors: [themeVariables.primary],
      fill: {
        opacity: .9
      },
      grid: {
        padding: {
          bottom: -4
        },
        borderColor: themeVariables.gridBorder,
        xaxis: {
          lines: {
            show: true
          }
        }
      },
      xaxis: {
        categories: categories,
        axisBorder: {
          color: themeVariables.gridBorder,
        },
        axisTicks: {
          color: themeVariables.gridBorder,
        },
        labels: {
          show: true,
          style: {
            colors: themeVariables.secondary,
          }
        }
      },
      yaxis: {
        title: {
          text: 'Number of Crimes',
          style:{
            size: 9,
            color: themeVariables.secondary
          }
        },
        labels: {
          offsetX: 0,
        },
      },
      legend: {
        show: true,
        position: "top",
        horizontalAlign: 'center',
        fontFamily: themeVariables.fontFamily,
        itemMargin: {
          horizontal: 8,
          vertical: 0
        },
      },
      stroke: {
        width: 0
      },
      dataLabels: {
        enabled: true,
        style: {
          fontSize: '10px',
          fontFamily: themeVariables.fontFamily,
        },
        offsetY: -27
      },
      plotOptions: {
        bar: {
          columnWidth: "50%",
          borderRadius: 4,
          dataLabels: {
            position: 'top',
            orientation: 'vertical',
          }
        },
      }
    }
  }
}
