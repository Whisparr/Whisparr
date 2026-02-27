function sortByName(a, b) {
  const nameA = a.name;
  const nameB = b.name;

  if (nameA < nameB) {
    return -1;
  }

  if (nameA > nameB) {
    return 1;
  }

  return 0;
}

export default sortByName;
