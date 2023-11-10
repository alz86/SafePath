import React, { useState } from 'react';
import { StyleSheet, Image } from 'react-native';
import MapLibreGL from '@maplibre/maplibre-react-native';
import { MAPLIBRE_TILESERVER_URL } from '@env';
import UserMapMarker from '../../assets/UserMapMarker.png';
import { get } from '../api/api';
import HospitalIcon from '../../assets/mapIcons/Hospital.png';
import StreetLampIcon from '../../assets/mapIcons/StreetLamp.png';
import CCTVIcon from '../../assets/mapIcons/CCTV.png';
import BusStationIcon from '../../assets/mapIcons/BusStation.png';
import RailWayStationIcon from '../../assets/mapIcons/RailwayStation.png';
import SemaphoreIcon from '../../assets/mapIcons/Semaphore.png';
import PoliceStationIcon from '../../assets/mapIcons/PoliceStation.png';

const mapIcons = {
  Hospital: HospitalIcon,
  StreetLamp: StreetLampIcon,
  CCTV: CCTVIcon,
  BusStation: BusStationIcon,
  RailWayStation: RailWayStationIcon,
  Semaphore: SemaphoreIcon,
  PoliceStation: PoliceStationIcon,
};

const MapLibreMap = ({
  shape,
  startPoint,
  userPosition,
  securityElements,
}: {
  shape?: any;
  startPoint: number[];
  userPosition?: number[];
  securityElements?: any;
}) => {
  return (
    <MapLibreGL.MapView
      style={styles.map}
      logoEnabled
      localizeLabels
      zoomEnabled
      scrollEnabled
      rotateEnabled={false}
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
      {securityElements && (
        <>
          <MapLibreGL.Images images={mapIcons} />
          <MapLibreGL.ShapeSource id="securityLayerSource" shape={securityElements}>
            <MapLibreGL.SymbolLayer
              id="securityLayer"
              style={{
                iconImage: ['get', 'type'],
              }}
            />
          </MapLibreGL.ShapeSource>
        </>
      )}
    </MapLibreGL.MapView>
  );
};

export default MapLibreMap;

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
