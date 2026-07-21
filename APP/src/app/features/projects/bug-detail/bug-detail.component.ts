import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BugService } from '../../../core/services/bug.service';
import { ProjectService } from '../../../core/services/project.service';
import {
  BugDetail,
  BUG_STATUSES,
  STATUS_LABELS,
  SEVERITY_COLORS,
  PRIORITY_COLORS
} from '../../../core/models/bug.model';
import { ProjectMember } from '../../../core/models/project.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';
import { CommentThreadComponent } from '../../comments/comment-thread/comment-thread.component';
import { AttachmentListComponent } from '../../../shared/components/attachment-list/attachment-list.component';

@Component({
  selector: 'app-bug-detail',
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    CommentThreadComponent,
    AttachmentListComponent
  ],
  templateUrl: './bug-detail.component.html',
  styleUrl: './bug-detail.component.scss'
})
export class BugDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private bugService = inject(BugService);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  canWrite = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.Developer, ROLES.QA]);
  canDelete = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  canAssignDeveloper = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  canAssignTester = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager, ROLES.QA]);
  statuses = BUG_STATUSES;
  statusLabels = STATUS_LABELS;
  severityColors = SEVERITY_COLORS;
  priorityColors = PRIORITY_COLORS;

  projectId = 0;
  bugId = 0;
  bug = signal<BugDetail | null>(null);
  members = signal<ProjectMember[]>([]);
  loading = signal(true);

  statusForm = this.fb.nonNullable.group({ status: ['Open'] });
  developerForm = this.fb.nonNullable.group({ developerId: [null as number | null] });
  testerForm = this.fb.nonNullable.group({ testerId: [null as number | null] });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.bugId = +this.route.snapshot.paramMap.get('bugId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members.set(m));
    this.loadBug();
  }

  loadBug(): void {
    this.loading.set(true);
    this.bugService.getBug(this.projectId, this.bugId).subscribe({
      next: b => {
        this.bug.set(b);
        this.statusForm.patchValue({ status: b.status });
        this.developerForm.patchValue({ developerId: b.developerId ?? null });
        this.testerForm.patchValue({ testerId: b.testerId ?? null });
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  updateStatus(): void {
    const status = this.statusForm.getRawValue().status;
    this.bugService.updateStatus(this.projectId, this.bugId, { status }).subscribe(b => this.bug.set(b));
  }

  assignDeveloper(): void {
    const developerId = this.developerForm.getRawValue().developerId;
    this.bugService.assignDeveloper(this.projectId, this.bugId, { developerId: developerId ?? undefined })
      .subscribe(b => this.bug.set(b));
  }

  assignTester(): void {
    const testerId = this.testerForm.getRawValue().testerId;
    this.bugService.assignTester(this.projectId, this.bugId, { testerId: testerId ?? undefined })
      .subscribe(b => this.bug.set(b));
  }

  deleteBug(): void {
    const b = this.bug();
    if (!b || !confirm(`Delete bug ${b.bugKey}?`)) return;
    this.bugService.deleteBug(this.projectId, this.bugId).subscribe(() => window.history.back());
  }
}
