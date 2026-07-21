import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { Attachment, AttachmentEntityType } from '../models/file.model';
import { UserProfile } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAttachments(entityType: AttachmentEntityType, entityId: number): Observable<Attachment[]> {
    return this.http
      .get<ApiResponse<Attachment[]>>(`${this.baseUrl}/files`, {
        params: { entityType, entityId: String(entityId) }
      })
      .pipe(map(r => this.unwrap(r)));
  }

  upload(file: File, entityType: AttachmentEntityType, entityId: number): Observable<Attachment> {
    const form = new FormData();
    form.append('file', file);
    form.append('entityType', entityType);
    form.append('entityId', String(entityId));
    return this.http
      .post<ApiResponse<Attachment>>(`${this.baseUrl}/files/upload`, form)
      .pipe(map(r => this.unwrap(r)));
  }

  uploadProfilePicture(file: File | Blob, fileName = 'profile.jpg'): Observable<UserProfile> {
    const form = new FormData();
    form.append('file', file, fileName);
    return this.http
      .post<ApiResponse<UserProfile>>(`${this.baseUrl}/files/profile-picture`, form)
      .pipe(map(r => this.unwrap(r)));
  }

  downloadBlob(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/files/${id}`, { responseType: 'blob' });
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<ApiResponse<void>>(`${this.baseUrl}/files/${id}`)
      .pipe(map(() => undefined));
  }

  loadImageObjectUrl(id: number): Observable<string> {
    return this.downloadBlob(id).pipe(
      map(blob => URL.createObjectURL(blob)),
      tap(url => {
        // Caller should revoke when done; attachment-list manages lifecycle
        void url;
      })
    );
  }

  extractFileIdFromUrl(url?: string | null): number | null {
    if (!url) return null;
    const match = url.match(/\/files\/(\d+)/);
    return match ? +match[1] : null;
  }

  private unwrap<T>(response: ApiResponse<T>): T {
    if (!response.success) {
      throw new Error(response.errors?.[0] ?? response.message ?? 'Request failed');
    }
    return response.data;
  }
}
