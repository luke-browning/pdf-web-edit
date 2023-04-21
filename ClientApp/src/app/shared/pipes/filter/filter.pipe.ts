import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'search'
})
export class SearchPipe implements PipeTransform {

  transform(items: any[], searchText: string, property: string): any[] {

    if (!items) {
      return [];
    }

    if (!searchText) {
      return items;
    }

    searchText = searchText.toLocaleLowerCase();

    return items.filter(it => {

      if (property) {
        return it[property].toLocaleLowerCase().indexOf(searchText) > -1;
      } else {
        return it.toLocaleLowerCase().indexOf(searchText) > -1;
      }
    });
  }

}
