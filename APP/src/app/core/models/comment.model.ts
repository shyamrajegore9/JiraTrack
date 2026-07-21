export type CommentEntityType = 'Task' | 'Bug';

export interface Mention {
  userId: number;
  userName: string;
  fullName: string;
}

export interface CommentReactionGroup {
  emoji: string;
  count: number;
  reactedByMe: boolean;
  userNames: string[];
}

export interface Comment {
  id: number;
  entityType: string;
  entityId: number;
  parentCommentId?: number;
  userId: number;
  userName: string;
  fullName: string;
  content: string;
  createdDate: string;
  updatedDate?: string;
  isEdited: boolean;
  canEdit: boolean;
  canDelete: boolean;
  mentions: Mention[];
  reactions: CommentReactionGroup[];
  replies: Comment[];
}

export interface CreateCommentRequest {
  entityType: CommentEntityType;
  entityId: number;
  parentCommentId?: number;
  content: string;
}

export interface UpdateCommentRequest {
  content: string;
}

export interface AddReactionRequest {
  emoji: string;
}

export const REACTION_EMOJIS = ['👍', '❤️', '😄', '🎉', '👀', '🚀'] as const;
