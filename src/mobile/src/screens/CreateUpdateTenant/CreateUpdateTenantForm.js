import { Ionicons } from '@expo/vector-icons';
import { useFormik } from 'formik';
import i18n from 'i18n-js';
import { Box, FormControl, Icon, Input, Stack } from 'native-base';
import PropTypes from 'prop-types';
import React, { useRef, useState } from 'react';
import * as Yup from 'yup';
import { FormButtons } from '../../components/FormButtons';
import ValidationMessage from '../../components/ValidationMessage/ValidationMessage';
import { usePermission } from '../../hooks/UsePermission';

const validations = {
  name: Yup.string().required('AbpAccount::ThisFieldIsRequired.'),
};

function CreateUpdateTenantForm({ editingTenant = {}, submit, remove }) {
  
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

CreateUpdateTenantForm.propTypes = {
  editingTenant: PropTypes.object,
  submit: PropTypes.func.isRequired,
  remove: PropTypes.func.isRequired,
};

export default CreateUpdateTenantForm;
