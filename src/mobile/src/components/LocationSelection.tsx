import React, { useState } from 'react';
import { View, Button } from 'react-native';
import PlacesAutocomplete from './PlacesAutocomplete';
import { LocationSelectionScreenProps } from '../navigation/types';
import { PlaceDetails } from '../types';

const LocationSelectionScreen: React.FC<LocationSelectionScreenProps> = ({ route, navigation }) => {
  const { type, onSelect } = route.params;
  const [details, setDetails] = useState<PlaceDetails>();

  const handleLocationSelect = () => {
    onSelect(details);
    navigation.goBack();
  };

  return (
    <View style={{ flex: 1, padding: 20 }}>
      <PlacesAutocomplete
        placeholder={`Type the ${type === 'start' ? 'starting location' : 'destination'}`}
        onPlaceSelected={setDetails}
      />
      <Button title="Continue" onPress={() => handleLocationSelect()} color="#7127E8" />
    </View>
  );
};

export default LocationSelectionScreen;
