import React from 'react';
import { StyleSheet } from 'react-native';
import MapLibreGL from '@maplibre/maplibre-react-native';
import { MAPLIBRE_TILESERVER_URL } from '@env';

const styles = StyleSheet.create({
  page: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF',
  },
  map: {
    flex: 1,
    alignSelf: 'stretch',
  },
});
const MapLibreMap = ({ shape, startPoint }: { shape?: any; startPoint: number[] }) => {
  return (
    <MapLibreGL.MapView
      style={styles.map}
      logoEnabled
      localizeLabels
      zoomEnabled
      scrollEnabled
      // rotateEnabled
      styleURL={MAPLIBRE_TILESERVER_URL}
    >
      <MapLibreGL.Camera zoomLevel={13} centerCoordinate={startPoint} />
      {shape ? (
        <MapLibreGL.ShapeSource id="routeShape" shape={shape}>
          <MapLibreGL.LineLayer id="routeLinearLayer" style={{ lineColor: 'blue', lineWidth: 5 }} />
        </MapLibreGL.ShapeSource>
      ) : null}
    </MapLibreGL.MapView>
  );
};

export default MapLibreMap;
