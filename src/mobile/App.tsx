import React from 'react';
import MapLibreGL from '@maplibre/maplibre-react-native';
import { SafeAreaView } from 'react-native';
import Home from './src/pages/home';
import AppNavigator from './src/navigation/AppNavigator';

// Code to be used when the app need to be optimized, hiding the splash
// once the app finished the init phase
/*
import * as SplashScreen from 'expo-splash-screen';
SplashScreen.preventAutoHideAsync()
  .catch(error => console.warn("expo-splash-screen preventAutoHideAsync error: ", error));
  useEffect(() => {
    async function prepare() {
      // ... do any preload/initialization tasks here ...
      // Once everything is set:
      await SplashScreen.hideAsync();
    }
  
    prepare();
  }, []);
  */

// Will be null for most users (only Mapbox authenticates this way).
// Required on Android. See Android installation notes.
MapLibreGL.setAccessToken(null);

export default function App() {
  return (
    <SafeAreaView style={{ flex: 1, paddingTop: 10 }}>
      <AppNavigator />
    </SafeAreaView>
  );
}
