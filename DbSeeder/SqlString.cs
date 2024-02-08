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
			drop table if exists vu_event_types;
			drop table if exists vu_speeches;

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
				event_type_id int,
				event_date date
			);

			create table vu_event_types
            (
                event_type_id int primary key,
				event_type text

            );

            INSERT INTO vu_event_types (event_type_id, event_type) VALUES 
				(1, 'one'),
                (2, 'two'),
                (3, 'three'),
                (4, 'four'),
                (5, 'five');

			create table vu_speeches
			(
				speech_id int,
				user_id bigint,
				speech_date date,
				speech_text text
			);


			SELECT create_distributed_table('vu_users', 'user_id');
			SELECT create_distributed_table('vu_events', 'user_id', colocate_with => 'vu_users');
			SELECT create_reference_table('vu_event_types');
			SELECT create_distributed_table('vu_speeches', 'user_id', colocate_with => 'vu_users');
	";

	}

}
