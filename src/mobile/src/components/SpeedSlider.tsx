import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import Slider from '@react-native-community/slider';

const SpeedSlider = ({
  onValueChanged,
  value,
}: {
  onValueChanged: (newValues: number) => void;
  value: number;
}) => (
  <View style={styles.container}>
    <View style={styles.labelsContainer}>
      <Text style={styles.sliderLabel}>Speed</Text>
      <Text style={styles.sliderLabel}>Safety</Text>
    </View>
    <Slider
      style={styles.slider}
      minimumValue={1}
      maximumValue={5}
      minimumTrackTintColor="#7127E8"
      maximumTrackTintColor="#E0E0E0"
      thumbTintColor="#7127E8"
      onValueChange={(newValue: number) => onValueChanged(newValue)}
      value={value}
    />
  </View>
);

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'white',
    width: '100%',
  },
  labelsContainer: {
    fontSize: 15,
    height: 20,
    flexDirection: 'row', // Set the children in a row direction
    justifyContent: 'space-between', // Push the children to the extremes
    alignItems: 'center', // Align the children vertically in the center (optional)
  },
  sliderLabel: {},
  slider: {
    height: 20,
  },
});

export default SpeedSlider;
