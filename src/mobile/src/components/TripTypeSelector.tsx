// TripTypeSelector.tsx
import React, { useState } from 'react';
import { View, StyleSheet } from 'react-native';
import ToggleButton from './ToggleButton';
import bike from '../../assets/bike.png';
import walk from '../../assets/walk.png';
import bus from '../../assets/bus.png';
import { TripType } from '../types';

const TripTypeSelector = ({
  onTripTypeSelected,
}: {
  onTripTypeSelected: (tripType: TripType) => void;
}) => {
  const [selectedType, setSelectedType] = useState<TripType>('walk');
  const handleButtonPress = (tripType: TripType) => {
    if (tripType === selectedType) return;
    setSelectedType(tripType);
    onTripTypeSelected(tripType);
  };

  return (
    <View style={styles.container}>
      <ToggleButton
        icon={walk}
        onPress={() => handleButtonPress('walk')}
        isSelected={selectedType === 'walk'}
      />
      <ToggleButton
        icon={bike}
        onPress={() => handleButtonPress('bike')}
        isSelected={selectedType === 'bike'}
      />
      <ToggleButton
        icon={bus}
        onPress={() => handleButtonPress('bus')}
        isSelected={selectedType === 'bus'}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: '100%',
    paddingHorizontal: '10%',
  },
});

export default TripTypeSelector;
