/* eslint-disable react/style-prop-object */
import { View, Button } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { useState } from 'react';
import PlacesAutocomplete from '../components/PlacesAutocomplete';
import MapLibreMap from '../components/MapLibreMap';
import { buildQueryString, post } from '../api/api';
import { PlaceDetails } from '../types';
import BottomSheetComponent from '../components/BottomSheetComponent';

// TODO: load init data from the  server
const mapStartingPoint = [13.405, 52.52]; // Berlin

const Home = () => {
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
    <>
      <MapLibreMap shape={route} startPoint={mapStartingPoint} userPosition={mapStartingPoint} />
      <BottomSheetComponent />
      <StatusBar style="auto" />
    </>
  );
};

export default Home;
