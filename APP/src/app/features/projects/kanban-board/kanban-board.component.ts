import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  CdkDrag,
  CdkDragDrop,
  CdkDropList,
  CdkDropListGroup,
  moveItemInArray,
  transferArrayItem
} from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { Subscription } from 'rxjs';
import { KanbanService } from '../../../core/services/kanban.service';
import { ProjectService } from '../../../core/services/project.service';
import { TaskService } from '../../../core/services/task.service';
import { KanbanBoard, KanbanCard, KanbanColumn, PRIORITY_COLORS } from '../../../core/models/kanban.model';
import { ProjectMember } from '../../../core/models/project.model';
import { LabelDto } from '../../../core/models/task.model';
import { SprintService } from '../../../core/services/sprint.service';
import { SprintListItem } from '../../../core/models/sprint.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-kanban-board',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    CdkDropListGroup,
    CdkDropList,
    CdkDrag,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatCardModule
  ],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss'
})
export class KanbanBoardComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private kanbanService = inject(KanbanService);
  private projectService = inject(ProjectService);
  private taskService = inject(TaskService);
  private sprintService = inject(SprintService);
  private tokenService = inject(TokenService);

  canWrite = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.Developer, ROLES.QA]);
  priorityColors = PRIORITY_COLORS;
  projectId = 0;
  board = signal<KanbanBoard | null>(null);
  members = signal<ProjectMember[]>([]);
  labels = signal<LabelDto[]>([]);
  sprints = signal<SprintListItem[]>([]);
  loading = signal(true);
  connected = signal(false);

  filterForm = this.fb.nonNullable.group({
    assigneeId: [null as number | null],
    labelId: [null as number | null],
    sprintId: [null as number | null]
  });

  private subs = new Subscription();

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members.set(m));
    this.taskService.getLabels(this.projectId).subscribe(l => this.labels.set(l));
    this.sprintService.getSprints(this.projectId).subscribe(s => this.sprints.set(s));
    this.loadBoard();
    this.kanbanService.connect(this.projectId);
    this.subs.add(this.kanbanService.boardChanged.subscribe(() => this.loadBoard(false)));
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.kanbanService.disconnect();
  }

  loadBoard(showSpinner = true): void {
    if (showSpinner) this.loading.set(true);
    const filter = this.filterForm.getRawValue();
    this.kanbanService.getBoard(this.projectId, {
      assigneeId: filter.assigneeId ?? undefined,
      labelId: filter.labelId ?? undefined,
      sprintId: filter.sprintId ?? undefined
    }).subscribe({
      next: board => {
        this.board.set(board);
        this.loading.set(false);
        this.connected.set(true);
      },
      error: () => this.loading.set(false)
    });
  }

  drop(event: CdkDragDrop<KanbanCard[]>, column: KanbanColumn): void {
    if (!this.canWrite) return;

    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      this.kanbanService.reorderCards(this.projectId, {
        status: column.status,
        taskIds: column.cards.map(c => c.id)
      }).subscribe({ error: () => this.loadBoard(false) });
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      const card = event.container.data[event.currentIndex];
      this.kanbanService.moveCard(this.projectId, {
        taskId: card.id,
        toStatus: column.status,
        newSortOrder: event.currentIndex
      }).subscribe({
        next: updated => {
          event.container.data[event.currentIndex] = updated;
        },
        error: () => this.loadBoard(false)
      });
    }
  }

  columnIds(columns: KanbanColumn[]): string[] {
    return columns.map(c => `column-${c.status}`);
  }
}
