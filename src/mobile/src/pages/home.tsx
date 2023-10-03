/* eslint-disable react/style-prop-object */
import { Image, TouchableOpacity, StyleSheet } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { useState } from 'react';
import MapLibreMap from '../components/MapLibreMap';
import { buildQueryString, post } from '../api/api';
import { PlaceDetails, SearchParams } from '../types';
import BottomSheetComponent from '../components/BottomSheetComponent';
import centerMapIcon from '../../assets/centerMap.png';

// TODO: load init data from the  server
const mapStartingPoint = [13.405, 52.52]; // Berlin

const Home = () => {
  const [route, setRoute] = useState<any>(null);

  const handleButtonPress = async (searchParams: SearchParams) => {
    const params = {
      sourceLatitude: searchParams.start.latitude,
      sourceLongitude: searchParams.start.longitude,
      destLatitude: searchParams.end.latitude,
      destLongitude: searchParams.end.longitude,
      safetyLevel: searchParams.safetyLevel,
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
      <TouchableOpacity style={styles.floatingButton}>
        <Image source={centerMapIcon} style={styles.imageStyle} />
      </TouchableOpacity>
      <BottomSheetComponent onSearch={handleButtonPress} />
      <StatusBar style="auto" />
    </>
  );
};

const styles = StyleSheet.create({
  floatingButton: {
    position: 'absolute',
    bottom: '20%',
    right: '5%',
    backgroundColor: 'white',
    padding: 10,
    borderRadius: 25,
    alignItems: 'center',
    justifyContent: 'center',
  },
  imageStyle: {
    width: 30,
    height: 30,
    resizeMode: 'contain',
  },
});

export default Home;
