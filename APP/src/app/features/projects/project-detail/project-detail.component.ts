import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectDetail } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-detail',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatTabsModule, MatIconModule, MatChipsModule],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss'
})
export class ProjectDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);

  project = signal<ProjectDetail | null>(null);
  projectId = 0;

  tabs = [
    { label: 'Overview', route: 'dashboard', icon: 'dashboard' },
    { label: 'Tasks', route: 'tasks', icon: 'task_alt' },
    { label: 'Kanban', route: 'kanban', icon: 'view_kanban' },
    { label: 'Sprints', route: 'sprints', icon: 'speed' },
    { label: 'Bugs', route: 'bugs', icon: 'bug_report' },
    { label: 'Members', route: 'members', icon: 'group' },
    { label: 'Settings', route: 'settings', icon: 'settings' }
  ];

  ngOnInit(): void {
    this.projectId = +this.route.snapshot.paramMap.get('projectId')!;
    this.loadProject();
  }

  loadProject(): void {
    this.projectService.getProject(this.projectId).subscribe(p => this.project.set(p));
  }
}
