import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectDashboard } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-dashboard',
  imports: [MatCardModule, MatIconModule],
  templateUrl: './project-dashboard.component.html',
  styleUrl: './project-dashboard.component.scss'
})
export class ProjectDashboardComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);

  dashboard = signal<ProjectDashboard | null>(null);

  ngOnInit(): void {
    const projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getDashboard(projectId).subscribe(d => this.dashboard.set(d));
  }
}
