import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { ProjectService } from '../../../core/services/project.service';
import { PROJECT_ROLES, ProjectMember } from '../../../core/models/project.model';
import { UserLookupDto } from '../../../core/models/user.model';
import { TokenService } from '../../../core/services/token.service';
import { ROLES } from '../../../core/constants/roles.constants';

@Component({
  selector: 'app-project-members',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatCardModule
  ],
  templateUrl: './project-members.component.html',
  styleUrl: './project-members.component.scss'
})
export class ProjectMembersComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private tokenService = inject(TokenService);

  canManage = this.tokenService.hasAnyRole([ROLES.Admin, ROLES.ProjectManager]);
  projectRoles = PROJECT_ROLES;
  members = signal<ProjectMember[]>([]);
  users = signal<UserLookupDto[]>([]);
  displayedColumns = ['fullName', 'email', 'projectRole', 'joinedDate', 'actions'];
  projectId = 0;

  addForm = this.fb.nonNullable.group({
    userId: [0, Validators.required],
    projectRole: ['Developer', Validators.required]
  });

  ngOnInit(): void {
    this.projectId = +this.route.parent!.snapshot.paramMap.get('projectId')!;
    this.loadMembers();
    if (this.canManage) {
      this.projectService.getUserLookup().subscribe(users => {
        this.users.set(users);
        if (users.length) this.addForm.patchValue({ userId: users[0].id });
      });
    }
  }

  loadMembers(): void {
    this.projectService.getMembers(this.projectId).subscribe(m => this.members.set(m));
  }

  addMember(): void {
    if (this.addForm.invalid) return;
    this.projectService.addMember(this.projectId, this.addForm.getRawValue()).subscribe(() => {
      this.loadMembers();
      this.addForm.patchValue({ projectRole: 'Developer' });
    });
  }

  removeMember(member: ProjectMember): void {
    if (!confirm(`Remove ${member.fullName} from project?`)) return;
    this.projectService.removeMember(this.projectId, member.userId).subscribe(() => this.loadMembers());
  }
}
