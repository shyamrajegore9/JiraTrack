import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { ProjectService } from '../../../core/services/project.service';
import { SprintService } from '../../../core/services/sprint.service';
import { ProjectListItem } from '../../../core/models/project.model';
import { SprintListItem } from '../../../core/models/sprint.model';

@Component({
  selector: 'app-sprint-report-selector',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule
  ],
  templateUrl: './sprint-report-selector.component.html',
  styleUrl: './sprint-report-selector.component.scss'
})
export class SprintReportSelectorComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private projectService = inject(ProjectService);
  private sprintService = inject(SprintService);

  projects = signal<ProjectListItem[]>([]);
  sprints = signal<SprintListItem[]>([]);

  form = this.fb.group({
    projectId: [null as number | null],
    sprintId: [null as number | null]
  });

  ngOnInit(): void {
    this.projectService.getProjects({ pageSize: 100 }).subscribe(r => this.projects.set(r.items));
    this.form.controls.projectId.valueChanges.subscribe(projectId => {
      this.form.patchValue({ sprintId: null });
      if (projectId) {
        this.sprintService.getSprints(projectId).subscribe(s => this.sprints.set(s));
      } else {
        this.sprints.set([]);
      }
    });
  }

  viewReport(): void {
    const { projectId, sprintId } = this.form.getRawValue();
    if (projectId && sprintId) {
      this.router.navigate(['/app/reports/sprint', sprintId], { queryParams: { projectId } });
    }
  }
}
