using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSeeder
{
	internal static class Strings
	{
		internal static string DbCreation = @"
			drop table if exists vu_users;
			drop table if exists vu_events;

			CREATE TABLE vu_users
			(
				user_id bigint primary key,
				name text,
				user_identifier text
			);


			-- creare primary key su event/user perchè user è distribution key
			CREATE TABLE vu_events
			(
				event_id bigint,
				user_id bigint, 
				event_type text,
				event_date TIMESTAMP
			);


			SELECT create_distributed_table('vu_users', 'user_id');
			SELECT create_distributed_table('vu_events', 'user_id', colocate_with => 'vu_users');
	";

		internal static string[] EventType = { "one", "two", "three", "four", "five" };
	}

}
