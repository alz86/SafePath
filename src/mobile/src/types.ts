/* eslint-disable max-classes-per-file */
export class PlaceDetails {
  public name!: string;

  public latitude!: number;

  public longitude!: number;
}

export type TripType = 'walk' | 'bike' | 'bus';

export class SearchParams {
  public start!: PlaceDetails;

  public end!: PlaceDetails;

  public safetyLevel!: number;
}
