/* eslint-disable react/style-prop-object */
import React, { useState } from 'react';
import { View, Button, SafeAreaView } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import MapLibreGL from '@maplibre/maplibre-react-native';
import PlacesAutocomplete, { PlaceDetails } from './src/components/PlacesAutocomplete';
import MapLibreMap from './src/components/MapLibreMap';
import { buildQueryString, post } from './src/api/api';

// Will be null for most users (only Mapbox authenticates this way).
// Required on Android. See Android installation notes.
MapLibreGL.setAccessToken(null);

const mapStartingPoint = [13.405, 52.52]; // Berlin

export default function App() {
  const [startPlace, setStartPlace] = useState<PlaceDetails | undefined>(undefined);
  const [endPlace, setEndPlace] = useState<PlaceDetails | undefined>(undefined);
  const [route, setRoute] = useState<any>(null);

  const handleButtonPress = async () => {
    if (!startPlace || !endPlace) return;

    const params = {
      sourceLatitude: startPlace.latitude,
      sourceLongitude: startPlace.longitude,
      destLatitude: endPlace.latitude,
      destLongitude: endPlace.longitude,
    };

    // test data
    // const params = {
    //   sourceLatitude: 52.51140710937834,
    //   sourceLongitude: 13.415687404804045,
    //   destLatitude: 52.505712002376185,
    //   destLongitude: 13.424840946638504,
    // };
    try {
      const url = `route-genereation/calculate-route?${buildQueryString(params)}`;
      const data = await post(url);
      setRoute(await data.json());
    } catch (e) {
      console.log('Exception called endpoint', e);
    }
  };

  return (
    <SafeAreaView style={{ flex: 1, paddingTop: 10 }}>
      <View style={{ flex: 0.3 }}>
        <PlacesAutocomplete
          placeholder="Enter Start Location"
          onPlaceSelected={(details) => setStartPlace(details)}
        />
      </View>
      <View style={{ flex: 0.3 }}>
        <PlacesAutocomplete
          placeholder="Enter End Location"
          onPlaceSelected={(details) => setEndPlace(details)}
        />
      </View>
      <MapLibreMap shape={route} startPoint={mapStartingPoint} />
      <Button title="Search" onPress={handleButtonPress} />
      <StatusBar style="auto" />
    </SafeAreaView>
  );
}
