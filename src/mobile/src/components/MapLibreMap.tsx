import React from 'react';
import { StyleSheet, Image } from 'react-native';
import MapLibreGL from '@maplibre/maplibre-react-native';
import { MAPLIBRE_TILESERVER_URL } from '@env';
import UserMapMarker from '../../assets/UserMapMarker.png';

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
const MapLibreMap = ({
  shape,
  startPoint,
  userPosition,
}: {
  shape?: any;
  startPoint: number[];
  userPosition?: number[];
}) => {
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
          <MapLibreGL.LineLayer
            id="routeLinearLayer"
            style={{
              lineColor: '#6177DB',
              lineWidth: 5,
              lineDasharray: [2, 1], // dashed pattern
            }}
          />
        </MapLibreGL.ShapeSource>
      ) : null}
      {userPosition && (
        <MapLibreGL.MarkerView id="userLocation" title="You are here" coordinate={userPosition}>
          <Image source={UserMapMarker} style={{ width: 32, height: 32 }} />
        </MapLibreGL.MarkerView>
      )}
    </MapLibreGL.MapView>
  );
};

export default MapLibreMap;
