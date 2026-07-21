import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommentService } from '../../../core/services/comment.service';
import { Comment, CommentEntityType } from '../../../core/models/comment.model';
import { CommentItemComponent } from '../comment-item/comment-item.component';

@Component({
  selector: 'app-comment-thread',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    CommentItemComponent
  ],
  templateUrl: './comment-thread.component.html',
  styleUrl: './comment-thread.component.scss'
})
export class CommentThreadComponent implements OnInit {
  private fb = inject(FormBuilder);
  private commentService = inject(CommentService);

  entityType = input.required<CommentEntityType>();
  entityId = input.required<number>();
  canWrite = input(true);

  comments = signal<Comment[]>([]);
  loading = signal(true);

  addForm = this.fb.nonNullable.group({
    content: ['', Validators.required]
  });

  ngOnInit(): void {
    this.loadComments();
  }

  loadComments(): void {
    this.loading.set(true);
    this.commentService.getComments(this.entityType(), this.entityId()).subscribe({
      next: c => {
        this.comments.set(c);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  addComment(): void {
    if (this.addForm.invalid) return;
    this.commentService.createComment({
      entityType: this.entityType(),
      entityId: this.entityId(),
      content: this.addForm.getRawValue().content
    }).subscribe(() => {
      this.addForm.reset({ content: '' });
      this.loadComments();
    });
  }
}
