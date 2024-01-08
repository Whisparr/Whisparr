import firstCharToUpper from './firstCharToUpper';

const regex = /([A-Z]+)/g;

function camelCaseToString(input) {
  if (!input) {
    return '';
  }

  return firstCharToUpper(input).replace(regex, ' $1').trim();
}

export default camelCaseToString;
