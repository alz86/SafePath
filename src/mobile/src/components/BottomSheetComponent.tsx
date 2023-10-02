import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Image, Dimensions, Button } from 'react-native';
import BottomSheet from '@gorhom/bottom-sheet';
import { useNavigation } from '@react-navigation/native';
import { StackNavigationProp } from '@react-navigation/stack';
import { PlaceDetails } from '../types';
import { LocationType, RootStackParamList } from '../navigation/types';
import startingPositionIcon from '../../assets/startingPositionIcon.png';
import destinationIcon from '../../assets/destinationIcon.png';
import TripTypeSelector from './TripTypeSelector';
import SpeedSlider from './SpeedSlider';

const screenWidth = Dimensions.get('window').width;

const BottomSheetComponent = () => {
  const navigation = useNavigation<StackNavigationProp<RootStackParamList>>();
  const [startLocation, setStartLocation] = useState<PlaceDetails | undefined>();
  const [destinationLocation, setDestinationLocation] = useState<PlaceDetails | undefined>();

  const handleSelectLocation = (type: LocationType) => {
    navigation.navigate('LocationSelection', {
      type: 'start',
      selectedValue: '',
      onSelect: (location: PlaceDetails | undefined) =>
        type === 'start' ? setStartLocation(location) : setDestinationLocation(location),
    });
  };

  const [sliderValue, setSliderValue] = useState<number>(1);

  return (
    <BottomSheet index={0} snapPoints={['15%', '60%']}>
      <View style={styles.container}>
        <TouchableOpacity
          onPress={() => handleSelectLocation('start')}
          style={styles.locationContainer}
        >
          <View style={styles.iconContainer}>
            <Image source={startingPositionIcon} style={styles.startIcon} />
          </View>
          <Text style={styles.label}>{startLocation?.name || 'Tap to set the starting point'}</Text>
        </TouchableOpacity>

        <TouchableOpacity
          onPress={() => handleSelectLocation('destination')}
          style={styles.locationContainer}
        >
          <View style={styles.iconContainer}>
            <Image source={destinationIcon} style={styles.destinationIcon} />
          </View>
          <Text style={styles.label}>{destinationLocation?.name || 'Tap to set Destination'}</Text>
        </TouchableOpacity>

        <View style={styles.transportOptions}>
          <TripTypeSelector onTripTypeSelected={(tripType) => {}} />
        </View>

        <View style={styles.sliderContainer}>
          <SpeedSlider onValueChanged={(v) => setSliderValue(v)} value={sliderValue} />
        </View>

        <View style={styles.buttonContainer}>
          <Button title="Search" color="#7127E8" />
        </View>
      </View>
    </BottomSheet>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'white',
    paddingHorizontal: '5%', // Using percentage for padding
  },
  locationContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: '2%',
    width: '100%',
  },
  iconContainer: {
    width: 30, // Width of the circle
    height: 30, // Height of the circle
    justifyContent: 'center',
    alignItems: 'center',
  },
  startIcon: {
    width: 15,
    height: 15,
    resizeMode: 'contain',
  },
  destinationIcon: {
    width: 15,
    height: 20,
    resizeMode: 'contain',
  },
  label: {
    padding: '3%', // Adjust this for desired padding
    borderRadius: 20,
    fontSize: 16,
    flex: 1,
    borderWidth: 1,
    borderColor: '#F9F9F9',
    backgroundColor: '#F9F9F9',
  },
  transportOptions: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginVertical: '5%',
    height: 100,
  },
  option: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  iconPlaceholder: {
    fontSize: 30,
    marginBottom: 10,
  },
  sliderContainer: {
    paddingTop: '5%', // Using percentage for padding
  },
  buttonContainer: {
    marginTop: 60,
  },
});

export default BottomSheetComponent;
