import { vi } from 'vitest'

import mockConstants from '../mock/constants.json'

vi.mock(
  '~root/data/constants.json',
  vi.fn().mockImplementation(() => mockConstants),
)
