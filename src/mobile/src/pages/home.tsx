/* eslint-disable react/style-prop-object */
import { Image, TouchableOpacity, StyleSheet } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { useState } from 'react';
import MapLibreMap from '../components/MapLibreMap';
import { buildQueryString, get, post } from '../api/api';
import { PlaceDetails, SearchParams } from '../types';
import BottomSheetComponent from '../components/BottomSheetComponent';
import centerMapIcon from '../../assets/centerMap.png';
import securityElements from '../../assets/SecurityElements.png';
import securityElementsSelected from '../../assets/SecurityElements-selected.png';

// TODO: load init data from the  server
// const mapStartingPoint = [13.405, 52.52]; // Berlin
const mapStartingPoint = [13.415769649582854, 52.51115003288883];

const Home = () => {
  const [route, setRoute] = useState<any>(null);
  const [elements, setElements] = useState(null);
  const [showElements, setShowElements] = useState(false);

  const handleButtonPress = async (searchParams: SearchParams) => {
    const params = {
      sourceLatitude: searchParams.start.latitude,
      sourceLongitude: searchParams.start.longitude,
      destLatitude: searchParams.end.latitude,
      destLongitude: searchParams.end.longitude,
      safetyLevel: searchParams.safetyLevel,
      profile: 1,
    };

    // test data
    // const params = {
    //   sourceLatitude: 52.51140710937834,
    //   sourceLongitude: 13.415687404804045,
    //   destLatitude: 52.505712002376185,
    //   destLongitude: 13.424840946638504,
    // };
    try {
      const url = `route-generation/calculate-route?${buildQueryString(params)}`;
      const data = await post(url);
      setRoute(await data.json());
    } catch (e) {
      console.log('Exception called endpoint', e);
    }
  };

  const initMapLayer = async () => {
    const data = await get(`area/security-layer-geo-jSON`);
    const featureCollection = await data.json();
    return featureCollection;
  };

  const handleSecElementsClick = async () => {
    setShowElements(!showElements);

    if (showElements && !elements) {
      try {
        setElements(await initMapLayer());
      } catch (e) {
        console.log('Exception called endpoint', e);
      }
    }
  };

  return (
    <>
      <MapLibreMap
        shape={route}
        startPoint={mapStartingPoint}
        userPosition={mapStartingPoint}
        securityElements={elements}
      />
      <TouchableOpacity
        style={[styles.floatingButton, styles.floatingButtonSecElements]}
        onPress={handleSecElementsClick}
      >
        <Image
          source={showElements ? securityElementsSelected : securityElements}
          style={styles.imageStyle}
        />
      </TouchableOpacity>
      <TouchableOpacity style={[styles.floatingButton, styles.floatingButtonCenter]}>
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
    right: '5%',
    backgroundColor: 'white',
    padding: 10,
    borderRadius: 25,
    alignItems: 'center',
    justifyContent: 'center',
  },
  floatingButtonCenter: {
    bottom: '20%',
  },
  floatingButtonSecElements: {
    bottom: '30%',
  },
  imageStyle: {
    width: 30,
    height: 30,
    resizeMode: 'contain',
  },
});

export default Home;
