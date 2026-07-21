import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddReactionRequest,
  Comment,
  CommentEntityType,
  CreateCommentRequest,
  UpdateCommentRequest
} from '../models/comment.model';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class CommentService {
  constructor(private api: ApiService) {}

  getComments(entityType: CommentEntityType, entityId: number): Observable<Comment[]> {
    return this.api.get<Comment[]>('/comments', { entityType, entityId });
  }

  createComment(request: CreateCommentRequest): Observable<Comment> {
    return this.api.post<Comment>('/comments', request);
  }

  updateComment(id: number, request: UpdateCommentRequest): Observable<Comment> {
    return this.api.put<Comment>(`/comments/${id}`, request);
  }

  deleteComment(id: number): Observable<void> {
    return this.api.delete<void>(`/comments/${id}`);
  }

  addReaction(id: number, request: AddReactionRequest): Observable<Comment> {
    return this.api.post<Comment>(`/comments/${id}/reactions`, request);
  }

  removeReaction(id: number, emoji: string): Observable<Comment> {
    return this.api.delete<Comment>(`/comments/${id}/reactions/${encodeURIComponent(emoji)}`);
  }
}
