let translations: Record<string, string> = {};

export function setTranslations(translationData: Record<string, string>) {
  translations = translationData;
}

export function translate(
  key: string,
  tokens: Record<string, string | number | boolean> = {}
) {
  const translation = translations[key] || key;

  tokens.appName = 'Whisparr';

  return translation.replace(/\{([a-z0-9]+?)\}/gi, (match, tokenMatch) =>
    String(tokens[tokenMatch] ?? match)
  );
}

export default translate;
