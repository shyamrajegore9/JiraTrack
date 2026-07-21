import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../core/services/user.service';
import { Role, UserListItem } from '../../../core/models/user.model';
import { PagedResponse } from '../../../core/models/api-response.model';

@Component({
  selector: 'app-user-list',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);

  displayedColumns = ['fullName', 'email', 'roles', 'isActive', 'actions'];
  users = signal<UserListItem[]>([]);
  roles = signal<Role[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  pageSize = 20;
  pageNumber = 1;

  filterForm = this.fb.nonNullable.group({
    searchTerm: [''],
    roleId: [null as number | null],
    isActive: [null as boolean | null]
  });

  ngOnInit(): void {
    this.userService.getRoles().subscribe((roles: Role[]) => this.roles.set(roles));
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    const filter = this.filterForm.getRawValue();
    this.userService.getUsers({
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: filter.searchTerm || undefined,
      roleId: filter.roleId ?? undefined,
      isActive: filter.isActive ?? undefined
    }).subscribe({
      next: (res: PagedResponse<UserListItem>) => {
        this.users.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPage(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  toggleActive(user: UserListItem): void {
    const action = user.isActive
      ? this.userService.deactivateUser(user.id)
      : this.userService.activateUser(user.id);

    action.subscribe(() => this.loadUsers());
  }

  deleteUser(user: UserListItem): void {
    if (!confirm(`Delete user ${user.fullName}?`)) return;
    this.userService.deleteUser(user.id).subscribe(() => this.loadUsers());
  }
}
