import { Injectable } from '@angular/core';

const STORAGE_KEY = 'jiratrack-recent-searches';
const MAX_RECENT = 8;

@Injectable({ providedIn: 'root' })
export class RecentSearchService {
  getRecent(): string[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) as string[] : [];
    } catch {
      return [];
    }
  }

  add(term: string): void {
    const trimmed = term.trim();
    if (trimmed.length < 2) return;

    const recent = this.getRecent().filter(t => t.toLowerCase() !== trimmed.toLowerCase());
    recent.unshift(trimmed);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(recent.slice(0, MAX_RECENT)));
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  }
}
