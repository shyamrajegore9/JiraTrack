import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { BugService } from '../../../core/services/bug.service';
import { ProjectService } from '../../../core/services/project.service';
import { ProjectMember } from '../../../core/models/project.model';
import { BUG_PRIORITIES, BUG_SEVERITIES, BUG_STATUSES, STATUS_LABELS } from '../../../core/models/bug.model';

@Component({
  selector: 'app-bug-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule
  ],
  templateUrl: './bug-form.component.html',
  styleUrl: './bug-form.component.scss'
})
export class BugFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private bugService = inject(BugService);
  private projectService = inject(ProjectService);

  isEdit = false;
  projectId = 0;
  bugId?: number;
  members: ProjectMember[] = [];
  loading = false;
  statuses = BUG_STATUSES;
  severities = BUG_SEVERITIES;
  priorities = BUG_PRIORITIES;
  statusLabels = STATUS_LABELS;

  form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(300)]],
    description: [''],
    status: ['Open'],
    severity: ['Medium'],
    priority: ['Medium'],
    environment: [''],
    browser: [''],
    operatingSystem: [''],
    stepsToReproduce: [''],
    expectedResult: [''],
    actualResult: [''],
    developerId: [null as number | null],
    testerId: [null as number | null]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.projectService.getMembers(this.projectId).subscribe(m => this.members = m);

    const bugIdParam = this.route.snapshot.paramMap.get('bugId');
    if (bugIdParam && bugIdParam !== 'new') {
      this.isEdit = true;
      this.bugId = +bugIdParam;
      this.form.controls.status.disable();
      this.bugService.getBug(this.projectId, this.bugId).subscribe(bug => {
        this.form.patchValue({
          title: bug.title,
          description: bug.description ?? '',
          status: bug.status,
          severity: bug.severity,
          priority: bug.priority,
          environment: bug.environment ?? '',
          browser: bug.browser ?? '',
          operatingSystem: bug.operatingSystem ?? '',
          stepsToReproduce: bug.stepsToReproduce ?? '',
          expectedResult: bug.expectedResult ?? '',
          actualResult: bug.actualResult ?? '',
          developerId: bug.developerId ?? null,
          testerId: bug.testerId ?? null
        });
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const value = this.form.getRawValue();

    if (this.isEdit && this.bugId) {
      this.bugService.updateBug(this.projectId, this.bugId, {
        title: value.title,
        description: value.description || undefined,
        severity: value.severity,
        priority: value.priority,
        environment: value.environment || undefined,
        browser: value.browser || undefined,
        operatingSystem: value.operatingSystem || undefined,
        stepsToReproduce: value.stepsToReproduce || undefined,
        expectedResult: value.expectedResult || undefined,
        actualResult: value.actualResult || undefined,
        developerId: value.developerId ?? undefined,
        testerId: value.testerId ?? undefined
      }).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'bugs', this.bugId]);
        },
        error: () => this.loading = false
      });
    } else {
      this.bugService.createBug(this.projectId, {
        title: value.title,
        description: value.description || undefined,
        status: value.status,
        severity: value.severity,
        priority: value.priority,
        environment: value.environment || undefined,
        browser: value.browser || undefined,
        operatingSystem: value.operatingSystem || undefined,
        stepsToReproduce: value.stepsToReproduce || undefined,
        expectedResult: value.expectedResult || undefined,
        actualResult: value.actualResult || undefined,
        developerId: value.developerId ?? undefined,
        testerId: value.testerId ?? undefined
      }).subscribe({
        next: bug => {
          this.loading = false;
          this.router.navigate(['/app/projects', this.projectId, 'bugs', bug.id]);
        },
        error: () => this.loading = false
      });
    }
  }
}
