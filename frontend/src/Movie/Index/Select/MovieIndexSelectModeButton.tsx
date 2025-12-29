import React, { useCallback } from 'react';
import { useSelect } from 'App/SelectContext';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';

type MovieIndexSelectModeButtonProps = React.ComponentProps<
  typeof PageToolbarButton
> & {
  isSelectMode: boolean;
  onPress: () => void;
  // Keep `any` here to match `PageToolbarButton`'s `overflowComponent` type
  // and avoid creating an intersection type that breaks assignment.
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  overflowComponent?: React.ComponentType<any>;
};

function MovieIndexSelectModeButton(props: MovieIndexSelectModeButtonProps) {
  const { label, iconName, isSelectMode, overflowComponent, onPress } = props;
  const [, selectDispatch] = useSelect();

  const onPressWrapper = useCallback(() => {
    if (isSelectMode) {
      selectDispatch({
        type: 'reset',
      });
    }

    onPress();
  }, [isSelectMode, onPress, selectDispatch]);

  return (
    <PageToolbarButton
      label={label}
      iconName={iconName}
      overflowComponent={overflowComponent}
      onPress={onPressWrapper}
    />
  );
}

export default MovieIndexSelectModeButton;
