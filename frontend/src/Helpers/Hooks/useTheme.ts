import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import themes from 'Styles/Themes';

type ThemeName = keyof typeof themes;

function createThemeSelector() {
  return createSelector(
    (state: AppState) =>
      (state.settings.ui.item.theme ||
        window.Whisparr.theme ||
        'auto') as ThemeName,
    (theme) => {
      return theme;
    }
  );
}

const useTheme = () => {
  return useSelector(createThemeSelector());
};

export default useTheme;

export const useThemeColor = (color: string) => {
  const theme = useTheme();
  const themeVariables = themes[theme];

  // @ts-expect-error - themeVariables is a string indexable type
  return themeVariables[color];
};
