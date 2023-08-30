import i18n from 'i18n-js';
import { Box, Center, Heading, Text } from 'native-base';
import { StyleSheet } from 'react-native';
import React, { useRef, useState } from 'react';


function HomeScreen() {
  const [start, setStart] = useState<string>('');
  const [end, setEnd] = useState<string>('');
  
  
  const onSubmit = values => {
    submit({
      ...editingTenant,
      ...values,
    });
  };

  return (
    <>
      <Box w={{ base: '100%' }} px="3">
        <FormControl isRequired my="2">
          <Stack mx="4">
            <FormControl.Label>Start point</FormControl.Label>
            <Input
              onChangeText={setStart}
              value={start}
              autoCapitalize="none"
              returnKeyType="next"
            />
          </Stack>
        </FormControl>
        <FormControl isRequired my="2">
          <Stack mx="4">
            <FormControl.Label>End point</FormControl.Label>
            <Input
              onChangeText={setEnd}
              value={end}
              autoCapitalize="none"
              returnKeyType="next"
            />
          </Stack>
        </FormControl>
      </Box>
      <FormButtons
        submit={onSubmit}
        // remove={remove}
        // removeMessage={i18n.t('AbpTenantManagement::TenantDeletionConfirmationMessage', {
        //   0: editingTenant.name,
        // })}
        // isSubmitDisabled={!formik.isValid}
        // isShowRemove={!!editingTenant.id && hasRemovePermission}
      />
    </>
  );
}

const styles = StyleSheet.create({
  centeredText: {
    textAlign: 'center',
    marginBottom: 5
  },
});

export default HomeScreen;
