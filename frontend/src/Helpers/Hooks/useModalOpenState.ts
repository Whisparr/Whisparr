import { useCallback, useState } from 'react';

export default function useModalOpenState(
  initialState: boolean
): [boolean, () => void, () => void] {
  const [isOpen, setIsOpen] = useState(initialState);

  const open = useCallback(() => {
    setIsOpen(true);
  }, []);

  const close = useCallback(() => {
    setIsOpen(false);
  }, []);

  return [isOpen, open, close];
}
