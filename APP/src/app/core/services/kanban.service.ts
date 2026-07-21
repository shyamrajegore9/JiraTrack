import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  KanbanBoard,
  KanbanCard,
  KanbanFilter,
  MoveKanbanCardRequest,
  ReorderKanbanCardsRequest
} from '../models/kanban.model';
import { ApiService } from './api.service';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class KanbanService implements OnDestroy {
  private connection?: HubConnection;
  private readonly boardChanged$ = new Subject<void>();
  private projectId = 0;

  constructor(
    private api: ApiService,
    private tokenService: TokenService
  ) {}

  get boardChanged(): Observable<void> {
    return this.boardChanged$.asObservable();
  }

  getBoard(projectId: number, filter: KanbanFilter): Observable<KanbanBoard> {
    return this.api.get<KanbanBoard>(`/projects/${projectId}/kanban`, filter as Record<string, string | number | boolean | undefined>);
  }

  moveCard(projectId: number, request: MoveKanbanCardRequest): Observable<KanbanCard> {
    return this.api.patch<KanbanCard>(`/projects/${projectId}/kanban/move`, request);
  }

  reorderCards(projectId: number, request: ReorderKanbanCardsRequest): Observable<void> {
    return this.api.patch<void>(`/projects/${projectId}/kanban/reorder`, request);
  }

  connect(projectId: number): void {
    if (this.connection) {
      this.disconnect();
    }

    this.projectId = projectId;
    const token = this.tokenService.getAccessToken() ?? '';

    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}/kanban`, {
        accessTokenFactory: () => this.tokenService.getAccessToken() ?? token
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('CardMoved', () => this.boardChanged$.next());
    this.connection.on('CardUpdated', () => this.boardChanged$.next());
    this.connection.on('CardAdded', () => this.boardChanged$.next());

    this.connection
      .start()
      .then(() => this.connection?.invoke('JoinProject', projectId))
      .catch(() => { /* board still works without real-time */ });
  }

  disconnect(): void {
    if (this.connection) {
      this.connection.invoke('LeaveProject', this.projectId).catch(() => undefined);
      this.connection.stop().catch(() => undefined);
      this.connection = undefined;
    }
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}
