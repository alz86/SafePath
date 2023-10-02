import { useRef } from 'react';
import { GooglePlacesAutocomplete } from 'react-native-google-places-autocomplete';
import { GOOGLE_CLOUD_KEY } from '@env';
import { PlaceDetails } from '../types';

// (navigator as any).geolocation = require('@react-native-community/geolocation');

const PlacesAutocomplete = ({
  placeholder,
  onPlaceSelected,
}: {
  placeholder: string;
  onPlaceSelected: (detail: PlaceDetails) => void;
}) => {
  const startLocationRef = useRef(null);
  return (
    <GooglePlacesAutocomplete
      ref={startLocationRef}
      placeholder={placeholder}
      numberOfLines={1}
      fetchDetails
      onPress={(data, details) => {
        // console.log('Places autocomplete', JSON.stringify({ data, details }));
        const placeDetails: PlaceDetails = {
          name: details?.name ?? '',
          latitude: details?.geometry.location.lat ?? 0,
          longitude: details?.geometry.location.lng ?? 0,
        };
        onPlaceSelected(placeDetails);
      }}
      query={{
        key: GOOGLE_CLOUD_KEY,
        language: 'en',
      }}
    />
  );
};

export default PlacesAutocomplete;
