export interface TimeSeries {
  name: string
  data: TimeSeriesItem[]
}

export type TimeSeriesItem = [Date, number]
