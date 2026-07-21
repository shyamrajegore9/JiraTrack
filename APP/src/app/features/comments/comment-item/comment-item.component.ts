import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommentService } from '../../../core/services/comment.service';
import { Comment, CommentEntityType, REACTION_EMOJIS } from '../../../core/models/comment.model';

@Component({
  selector: 'app-comment-item',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    CommentItemComponent
  ],
  templateUrl: './comment-item.component.html',
  styleUrl: './comment-item.component.scss'
})
export class CommentItemComponent implements OnInit {
  private fb = inject(FormBuilder);
  private commentService = inject(CommentService);

  comment = input.required<Comment>();
  entityType = input.required<CommentEntityType>();
  entityId = input.required<number>();
  canWrite = input(true);
  depth = input(0);
  changed = output<void>();

  reactionEmojis = REACTION_EMOJIS;
  editing = signal(false);
  replying = signal(false);

  editForm = this.fb.nonNullable.group({ content: ['', Validators.required] });
  replyForm = this.fb.nonNullable.group({ content: ['', Validators.required] });

  ngOnInit(): void {
    this.editForm.patchValue({ content: this.comment().content });
  }

  startEdit(): void {
    this.editForm.patchValue({ content: this.comment().content });
    this.editing.set(true);
  }

  saveEdit(): void {
    if (this.editForm.invalid) return;
    this.commentService.updateComment(this.comment().id, this.editForm.getRawValue()).subscribe(() => {
      this.editing.set(false);
      this.changed.emit();
    });
  }

  deleteComment(): void {
    if (!confirm('Delete this comment?')) return;
    this.commentService.deleteComment(this.comment().id).subscribe(() => this.changed.emit());
  }

  submitReply(): void {
    if (this.replyForm.invalid) return;
    this.commentService.createComment({
      entityType: this.entityType(),
      entityId: this.entityId(),
      parentCommentId: this.comment().id,
      content: this.replyForm.getRawValue().content
    }).subscribe(() => {
      this.replyForm.reset({ content: '' });
      this.replying.set(false);
      this.changed.emit();
    });
  }

  toggleReaction(emoji: string): void {
    const group = this.comment().reactions.find(r => r.emoji === emoji);
    const action = group?.reactedByMe
      ? this.commentService.removeReaction(this.comment().id, emoji)
      : this.commentService.addReaction(this.comment().id, { emoji });
    action.subscribe(() => this.changed.emit());
  }

  canReply(): boolean {
    return this.canWrite() && this.depth() < 2;
  }
}
