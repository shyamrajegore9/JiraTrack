import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectListItem } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-report-selector',
  imports: [RouterLink, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatSelectModule, MatIconModule],
  templateUrl: './project-report-selector.component.html',
  styleUrl: './project-report-selector.component.scss'
})
export class ProjectReportSelectorComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private projectService = inject(ProjectService);

  projects = signal<ProjectListItem[]>([]);
  form = this.fb.group({ projectId: [null as number | null] });

  ngOnInit(): void {
    this.projectService.getProjects({ pageSize: 100 }).subscribe(r => this.projects.set(r.items));
  }

  viewReport(): void {
    const projectId = this.form.value.projectId;
    if (projectId) this.router.navigate(['/app/reports/project', projectId]);
  }
}
