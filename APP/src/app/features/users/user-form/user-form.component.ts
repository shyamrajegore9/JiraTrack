import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { UserService } from '../../../core/services/user.service';
import { Role } from '../../../core/models/user.model';

@Component({
  selector: 'app-user-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private userService = inject(UserService);

  isEdit = false;
  userId?: number;
  roles: Role[] = [];
  loading = false;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    userName: ['', [Validators.required, Validators.minLength(3)]],
    password: [''],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: [''],
    timeZone: ['UTC'],
    roleIds: [[] as number[], Validators.required]
  });

  ngOnInit(): void {
    this.userService.getRoles().subscribe(roles => {
      this.roles = roles;

      const id = this.route.snapshot.paramMap.get('id');
      if (id && id !== 'new') {
        this.isEdit = true;
        this.userId = +id;
        this.form.controls.password.clearValidators();
        this.userService.getUser(this.userId).subscribe(user => {
          this.form.patchValue({
            email: user.email,
            userName: user.userName,
            firstName: user.firstName,
            lastName: user.lastName,
            phoneNumber: user.phoneNumber ?? '',
            timeZone: user.timeZone,
            roleIds: this.roles.filter(r => user.roles.includes(r.name)).map(r => r.id)
          });
        });
      } else {
        this.form.controls.password.setValidators([Validators.required, Validators.minLength(8)]);
      }
      this.form.controls.password.updateValueAndValidity();
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const value = this.form.getRawValue();

    if (this.isEdit && this.userId) {
      this.userService.updateUser(this.userId, {
        email: value.email,
        userName: value.userName,
        firstName: value.firstName,
        lastName: value.lastName,
        phoneNumber: value.phoneNumber || undefined,
        timeZone: value.timeZone
      }).subscribe({
        next: () => {
          this.userService.assignRoles(this.userId!, { roleIds: value.roleIds }).subscribe({
            next: () => {
              this.loading = false;
              this.router.navigate(['/app/users']);
            },
            error: () => this.loading = false
          });
        },
        error: () => this.loading = false
      });
    } else {
      this.userService.createUser({
        email: value.email,
        userName: value.userName,
        password: value.password,
        firstName: value.firstName,
        lastName: value.lastName,
        phoneNumber: value.phoneNumber || undefined,
        roleIds: value.roleIds
      }).subscribe({
        next: () => {
          this.loading = false;
          this.router.navigate(['/app/users']);
        },
        error: () => this.loading = false
      });
    }
  }
}
