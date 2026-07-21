import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SprintService } from '../../../core/services/sprint.service';
import { SprintListItem, STATUS_LABELS, STATUS_COLORS } from '../../../core/models/sprint.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-sprint-list',
  imports: [
    DatePipe,
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './sprint-list.component.html',
  styleUrl: './sprint-list.component.scss'
})
export class SprintListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private sprintService = inject(SprintService);
  private tokenService = inject(TokenService);

  canManage = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  statusLabels = STATUS_LABELS;
  statusColors = STATUS_COLORS;
  projectId = 0;
  sprints = signal<SprintListItem[]>([]);
  loading = signal(true);
  displayedColumns = ['name', 'status', 'dates', 'tasks', 'points', 'actions'];

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.loadSprints();
  }

  loadSprints(): void {
    this.loading.set(true);
    this.sprintService.getSprints(this.projectId).subscribe({
      next: s => {
        this.sprints.set(s);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  deleteSprint(sprint: SprintListItem): void {
    if (!confirm(`Delete sprint "${sprint.name}"?`)) return;
    this.sprintService.deleteSprint(this.projectId, sprint.id).subscribe(() => this.loadSprints());
  }
}
