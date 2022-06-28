import { BaseEntity } from './Base/BaseEntity'
import { Role } from './Role'

export interface IUser extends BaseEntity<number> {
  name: string
  birthday?: Date
  roles: Role[]
}