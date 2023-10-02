import React from 'react';
import { TouchableOpacity, Image, StyleSheet } from 'react-native';

const ToggleButton = ({
  icon,
  onPress,
  isSelected: isActive,
}: {
  icon: any;
  isSelected: boolean;
  onPress: () => void;
}) => (
  <TouchableOpacity
    style={[styles.buttonContainer, isActive && styles.activeStyle]}
    onPress={() => onPress()}
  >
    <Image source={icon} style={styles.icon} />
  </TouchableOpacity>
);

const styles = StyleSheet.create({
  buttonContainer: {
    width: 50,
    height: 50,
    borderRadius: 25,
    alignItems: 'center',
    justifyContent: 'center',
    margin: 10,
    backgroundColor: '#E0E0E0', // Default Gray background
  },
  activeStyle: {
    backgroundColor: '#7127E8', // Purple background
  },
  icon: {
    width: '60%',
    height: '60%',
    resizeMode: 'contain',
  },
});

export default ToggleButton;
