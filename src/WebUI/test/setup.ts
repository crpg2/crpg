import { vi } from 'vitest'

import mockConstants from '../mocks/constants.json'

vi.mock('~root/data/constants.json', vi.fn().mockImplementation(() => mockConstants))
