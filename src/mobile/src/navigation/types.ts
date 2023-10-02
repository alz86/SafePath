import { StackScreenProps } from '@react-navigation/stack';
import { PlaceDetails } from '../types';

export type LocationType = 'start' | 'destination';

export type RootStackParamList = {
  Home: undefined;
  LocationSelection: {
    type: LocationType;
    onSelect: (location: PlaceDetails | undefined) => void;
    selectedValue?: string;
  };
};

export type LocationSelectionScreenProps = StackScreenProps<
  RootStackParamList,
  'LocationSelection'
>;
