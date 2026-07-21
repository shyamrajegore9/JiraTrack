import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'highlight' })
export class HighlightPipe implements PipeTransform {
  transform(value: string | null | undefined, term: string): string {
    if (!value || !term?.trim()) return value ?? '';

    const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return value.replace(new RegExp(`(${escaped})`, 'gi'), '<mark>$1</mark>');
  }
}
